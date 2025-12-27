import http from 'k6/http';
import { check, sleep } from 'k6';
import encoding from 'k6/encoding';
import { uuidv4, randomString } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

const BASE_URL = __ENV.K6_BASE_URL || 'http://api.fireinvent.de';
const STATIC_TOKEN = __ENV.K6_TOKEN;
const TOKEN_URL = __ENV.K6_OAUTH_TOKEN_URL; // e.g. https://auth.fireinvent.de/realms/<realm>/protocol/openid-connect/token
const CLIENT_ID = __ENV.K6_CLIENT_ID;
const CLIENT_SECRET = __ENV.K6_CLIENT_SECRET;

let tokenCache = {
  token: STATIC_TOKEN || null,
  exp: 0,
};

export const options = {
  scenarios: {
    write_heavy: {
      executor: 'constant-arrival-rate',
      rate: Number(__ENV.K6_RATE || 10),
      timeUnit: '1s',
      duration: __ENV.K6_DURATION || '5m',
      preAllocatedVUs: Number(__ENV.K6_PREALLOCATED_VUS || 20),
      maxVUs: Number(__ENV.K6_MAX_VUS || 200),
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<1500'],
  },
};

function fetchOAuthToken() {
  if (!TOKEN_URL || !CLIENT_ID || !CLIENT_SECRET) {
    return null;
  }

  const payload = {
    grant_type: 'client_credentials',
    client_id: CLIENT_ID,
    client_secret: CLIENT_SECRET,
  };

  const res = http.post(TOKEN_URL, payload, {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  });

  check(res, {
    'oauth token 200': (r) => r.status === 200,
  });

  if (res.status !== 200) {
    throw new Error(`OAuth token request failed: ${res.status}`);
  }

  const body = res.json();
  const accessToken = body?.access_token;
  const expiresIn = body?.expires_in || 60;

  if (!accessToken) {
    throw new Error('OAuth token response missing access_token');
  }

  const now = Date.now();
  // refresh a bit early (5s safety window)
  tokenCache = {
    token: accessToken,
    exp: now + Math.max(0, (expiresIn - 5) * 1000),
  };

  return accessToken;
}

function getAuthToken() {
  const now = Date.now();
  if (tokenCache.token && tokenCache.exp > now) {
    return tokenCache.token;
  }

  const oauthToken = fetchOAuthToken();
  if (oauthToken) {
    return oauthToken;
  }

  if (STATIC_TOKEN) {
    return STATIC_TOKEN;
  }

  throw new Error('Provide either K6_TOKEN or OAuth2 env vars (K6_OAUTH_TOKEN_URL, K6_CLIENT_ID, K6_CLIENT_SECRET).');
}

function jsonHeaders() {
  const token = getAuthToken();
  return {
    headers: {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    },
  };
}

function is2xx(status) {
  return status >= 200 && status < 300;
}

function idFrom(res) {
  try {
    const body = res.json();
    if (body && body.id) return body.id;
  } catch (e) {
    // ignore JSON parse errors
  }
  const location = res.headers?.Location || res.headers?.location;
  if (location) return location.split('/').pop();
  return null;
}

function postJson(path, payload) {
  const res = http.post(`${BASE_URL}${path}`, JSON.stringify(payload), jsonHeaders());
  check(res, {
    [`${path} responded 2xx`]: (r) => is2xx(r.status),
  });
  return { res, id: idFrom(res) };
}

function isoPlusMinutes(mins) {
  return new Date(Date.now() + mins * 60_000).toISOString();
}

function getUserIdFromToken() {
  const token = getAuthToken();
  if (!token) {
    throw new Error('No token available to extract user ID');
  }
  
  // JWT consists of three parts: header.payload.signature
  const parts = token.split('.');
  if (parts.length !== 3) {
    throw new Error('Invalid JWT token format');
  }
  
  // Decode the payload (second part)
  // JWT uses base64url encoding, but k6's encoding.b64decode expects standard base64
  // We need to convert base64url to base64
  let payload = parts[1];
  payload = payload.replace(/-/g, '+').replace(/_/g, '/');
  
  // Add padding if needed
  while (payload.length % 4 !== 0) {
    payload += '=';
  }
  
  try {
    const decoded = encoding.b64decode(payload, 'rawstd');
    const payloadObj = JSON.parse(decoded);
    return payloadObj.sub;
  } catch (e) {
    throw new Error(`Failed to decode JWT: ${e}`);
  }
}

export default function () {
  const suffix = uuidv4();
  const auth = jsonHeaders();
  const createdResources = [];

  // === PHASE 1: Browse/List Operations (Many GETs) ===
  // Simulate user exploring the system
  
  let res = http.get(`${BASE_URL}/departments`, auth);
  check(res, { 'list departments 2xx': (r) => is2xx(r.status) });
  sleep(0.3);

  res = http.get(`${BASE_URL}/persons`, auth);
  check(res, { 'list persons 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  res = http.get(`${BASE_URL}/manufacturers`, auth);
  check(res, { 'list manufacturers 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  res = http.get(`${BASE_URL}/product-types`, auth);
  check(res, { 'list product-types 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  res = http.get(`${BASE_URL}/products`, auth);
  check(res, { 'list products 2xx': (r) => is2xx(r.status) });
  sleep(0.3);

  res = http.get(`${BASE_URL}/storage-locations`, auth);
  check(res, { 'list storage-locations 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  res = http.get(`${BASE_URL}/items`, auth);
  check(res, { 'list items 2xx': (r) => is2xx(r.status) });
  sleep(0.3);

  // === PHASE 2: Create Master Data (POSTs) ===
  
  const dept = postJson('/departments', {
    name: `Dept-${suffix}`,
    description: 'k6 load test department',
  });
  if (dept.id) createdResources.push({ type: 'department', id: dept.id });
  sleep(0.2);

  const person = postJson('/persons', {
    firstName: `Load${randomString(5)}`,
    lastName: `Tester${randomString(5)}`,
    departmentIds: dept.id ? [dept.id] : [],
  });
  if (person.id) createdResources.push({ type: 'person', id: person.id });
  sleep(0.2);

  const manufacturer = postJson('/manufacturers', {
    name: `Maker-${suffix}`,
    city: 'k6-city',
    country: 'Germany',
  });
  if (manufacturer.id) createdResources.push({ type: 'manufacturer', id: manufacturer.id });
  sleep(0.2);

  const productType = postJson('/product-types', {
    name: `Type-${suffix}`,
    description: 'Load test product type',
  });
  if (productType.id) createdResources.push({ type: 'productType', id: productType.id });
  sleep(0.2);

  const maintenanceType = postJson('/maintenance-types', {
    name: `Maintenance-${suffix}`,
    description: 'Regular inspection',
  });
  if (maintenanceType.id) createdResources.push({ type: 'maintenanceType', id: maintenanceType.id });
  sleep(0.2);

  const product = postJson('/products', {
    name: `Product-${suffix}`,
    manufacturerId: manufacturer.id,
    typeId: productType.id,
    description: 'Test product for load testing',
  });
  if (product.id) createdResources.push({ type: 'product', id: product.id });
  sleep(0.2);

  const variant = postJson('/variants', {
    productId: product.id,
    name: `Variant-${suffix}`,
    additionalSpecs: 'Standard configuration',
  });
  if (variant.id) createdResources.push({ type: 'variant', id: variant.id });
  sleep(0.2);

  const storage = postJson('/storage-locations', {
    name: `Storage-${suffix}`,
    remarks: 'k6 test location',
  });
  if (storage.id) createdResources.push({ type: 'storage', id: storage.id });
  sleep(0.2);

  // === PHASE 3: View Created Resources (GETs) ===
  
  if (dept.id) {
    res = http.get(`${BASE_URL}/departments/${dept.id}`, auth);
    check(res, { 'get department 2xx': (r) => is2xx(r.status) });
    sleep(0.2);

    res = http.get(`${BASE_URL}/departments/${dept.id}/persons`, auth);
    check(res, { 'get department persons 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (person.id) {
    res = http.get(`${BASE_URL}/persons/${person.id}`, auth);
    check(res, { 'get person 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (manufacturer.id) {
    res = http.get(`${BASE_URL}/manufacturers/${manufacturer.id}`, auth);
    check(res, { 'get manufacturer 2xx': (r) => is2xx(r.status) });
    sleep(0.2);

    res = http.get(`${BASE_URL}/manufacturers/${manufacturer.id}/products`, auth);
    check(res, { 'get manufacturer products 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (product.id) {
    res = http.get(`${BASE_URL}/products/${product.id}`, auth);
    check(res, { 'get product 2xx': (r) => is2xx(r.status) });
    sleep(0.2);

    res = http.get(`${BASE_URL}/products/${product.id}/variants`, auth);
    check(res, { 'get product variants 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (variant.id) {
    res = http.get(`${BASE_URL}/variants/${variant.id}`, auth);
    check(res, { 'get variant 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  // === PHASE 4: Create Items & Operations (More POSTs) ===
  
  const item = postJson('/items', {
    variantId: variant.id,
    condition: 'New',
    purchaseDate: isoPlusMinutes(-60),
    storageLocationId: storage.id,
    identifier: `ITEM-${suffix}`,
  });
  if (item.id) createdResources.push({ type: 'item', id: item.id });
  sleep(0.2);

  if (item.id) {
    res = http.get(`${BASE_URL}/items/${item.id}`, auth);
    check(res, { 'get item 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  // Create order
  const order = postJson('/orders', {
    orderDate: isoPlusMinutes(-30),
    status: 0,
    orderIdentifier: `ORD-${suffix}`,
  });
  if (order.id) createdResources.push({ type: 'order', id: order.id });
  sleep(0.2);

  const orderItem = postJson('/order-items', {
    orderId: order.id,
    variantId: variant.id,
    quantity: 5,
  });
  if (orderItem.id) createdResources.push({ type: 'orderItem', id: orderItem.id });
  sleep(0.2);

  if (order.id) {
    res = http.get(`${BASE_URL}/orders/${order.id}`, auth);
    check(res, { 'get order 2xx': (r) => is2xx(r.status) });
    sleep(0.2);

    res = http.get(`${BASE_URL}/order-items/by-order/${order.id}`, auth);
    check(res, { 'get order items 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  // Create assignment
  const userId = getUserIdFromToken();
  const assignment = postJson('/assignments', {
    itemId: item.id,
    personId: person.id,
    assignedById: userId,
    assignedFrom: isoPlusMinutes(-10),
  });
  if (assignment.id) createdResources.push({ type: 'assignment', id: assignment.id });
  sleep(0.2);

  if (item.id) {
    res = http.get(`${BASE_URL}/items/${item.id}/assignments`, auth);
    check(res, { 'get item assignments 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (person.id) {
    res = http.get(`${BASE_URL}/persons/${person.id}/assignments`, auth);
    check(res, { 'get person assignments 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  // Create maintenance record
  const maintenance = postJson('/maintenances', {
    itemId: item.id,
    performedAt: isoPlusMinutes(-5),
    typeId: maintenanceType.id,
    performedById: userId,
    remarks: 'Regular check',
  });
  if (maintenance.id) createdResources.push({ type: 'maintenance', id: maintenance.id });
  sleep(0.2);

  if (item.id) {
    res = http.get(`${BASE_URL}/items/${item.id}/maintenance`, auth);
    check(res, { 'get item maintenance 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  // Create appointment and visit
  const appointment = postJson('/appointments', {
    scheduledAt: isoPlusMinutes(60),
    description: 'k6 load test appointment',
  });
  if (appointment.id) createdResources.push({ type: 'appointment', id: appointment.id });
  sleep(0.2);

  const visit = postJson('/visits', {
    appointmentId: appointment.id,
    personId: person.id,
  });
  if (visit.id) createdResources.push({ type: 'visit', id: visit.id });
  sleep(0.2);

  const visitItem = postJson('/visit-items', {
    visitId: visit.id,
    productId: product.id,
    quantity: 2,
  });
  if (visitItem.id) createdResources.push({ type: 'visitItem', id: visitItem.id });
  sleep(0.2);

  if (appointment.id) {
    res = http.get(`${BASE_URL}/appointments/${appointment.id}`, auth);
    check(res, { 'get appointment 2xx': (r) => is2xx(r.status) });
    sleep(0.2);

    res = http.get(`${BASE_URL}/appointments/${appointment.id}/visits`, auth);
    check(res, { 'get appointment visits 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (visit.id) {
    res = http.get(`${BASE_URL}/visits/${visit.id}`, auth);
    check(res, { 'get visit 2xx': (r) => is2xx(r.status) });
    sleep(0.2);

    res = http.get(`${BASE_URL}/visits/${visit.id}/items`, auth);
    check(res, { 'get visit items 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (storage.id) {
    res = http.get(`${BASE_URL}/storage-locations/${storage.id}/items`, auth);
    check(res, { 'get storage items 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  // === PHASE 5: Update Operations (PUTs) ===
  
  if (dept.id) {
    res = http.put(
      `${BASE_URL}/departments/${dept.id}`,
      JSON.stringify({ name: `Updated-Dept-${suffix}`, description: 'Updated description' }),
      jsonHeaders()
    );
    check(res, { 'update department 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (person.id) {
    res = http.put(
      `${BASE_URL}/persons/${person.id}`,
      JSON.stringify({
        firstName: `UpdatedLoad${randomString(5)}`,
        lastName: `UpdatedTester${randomString(5)}`,
        departmentIds: dept.id ? [dept.id] : [],
      }),
      jsonHeaders()
    );
    check(res, { 'update person 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (item.id) {
    res = http.put(
      `${BASE_URL}/items/${item.id}`,
      JSON.stringify({
        variantId: variant.id,
        condition: 'Used',
        purchaseDate: isoPlusMinutes(-60),
        storageLocationId: storage.id,
        identifier: `UPDATED-ITEM-${suffix}`,
      }),
      jsonHeaders()
    );
    check(res, { 'update item 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (order.id) {
    res = http.put(
      `${BASE_URL}/orders/${order.id}`,
      JSON.stringify({
        orderDate: isoPlusMinutes(-30),
        status: 'Delivered',
        orderIdentifier: `ORD-${suffix}`,
        deliveryDate: isoPlusMinutes(-1),
      }),
      jsonHeaders()
    );
    check(res, { 'update order 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (assignment.id) {
    res = http.put(
      `${BASE_URL}/assignments/${assignment.id}`,
      JSON.stringify({
        itemId: item.id,
        personId: person.id,
        assignedById: userId,
        assignedFrom: isoPlusMinutes(-10),
        assignedUntil: isoPlusMinutes(30),
      }),
      jsonHeaders()
    );
    check(res, { 'update assignment 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  if (appointment.id) {
    res = http.put(
      `${BASE_URL}/appointments/${appointment.id}`,
      JSON.stringify({
        scheduledAt: isoPlusMinutes(90),
        description: 'Updated appointment description',
      }),
      jsonHeaders()
    );
    check(res, { 'update appointment 2xx': (r) => is2xx(r.status) });
    sleep(0.2);
  }

  // === PHASE 6: Final Reads (More GETs) ===
  
  res = http.get(`${BASE_URL}/assignments`, auth);
  check(res, { 'list assignments 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  res = http.get(`${BASE_URL}/maintenances`, auth);
  check(res, { 'list maintenances 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  res = http.get(`${BASE_URL}/orders`, auth);
  check(res, { 'list orders 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  res = http.get(`${BASE_URL}/visits`, auth);
  check(res, { 'list visits 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  res = http.get(`${BASE_URL}/appointments`, auth);
  check(res, { 'list appointments 2xx': (r) => is2xx(r.status) });
  sleep(0.2);

  // === PHASE 7: Cleanup (DELETEs) ===
  // Delete in reverse order to respect dependencies
  
  for (let i = createdResources.length - 1; i >= 0; i--) {
    const resource = createdResources[i];
    let endpoint = '';
    
    switch (resource.type) {
      case 'visitItem':
        endpoint = `/visit-items/${resource.id}`;
        break;
      case 'visit':
        endpoint = `/visits/${resource.id}`;
        break;
      case 'appointment':
        endpoint = `/appointments/${resource.id}`;
        break;
      case 'maintenance':
        endpoint = `/maintenances/${resource.id}`;
        break;
      case 'assignment':
        endpoint = `/assignments/${resource.id}`;
        break;
      case 'orderItem':
        endpoint = `/order-items/${resource.id}`;
        break;
      case 'order':
        endpoint = `/orders/${resource.id}`;
        break;
      case 'item':
        endpoint = `/items/${resource.id}`;
        break;
      case 'variant':
        endpoint = `/variants/${resource.id}`;
        break;
      case 'product':
        endpoint = `/products/${resource.id}`;
        break;
      case 'storage':
        endpoint = `/storage-locations/${resource.id}`;
        break;
      case 'maintenanceType':
        endpoint = `/maintenance-types/${resource.id}`;
        break;
      case 'productType':
        endpoint = `/product-types/${resource.id}`;
        break;
      case 'manufacturer':
        endpoint = `/manufacturers/${resource.id}`;
        break;
      case 'person':
        endpoint = `/persons/${resource.id}`;
        break;
      case 'department':
        endpoint = `/departments/${resource.id}`;
        break;
    }
    
    if (endpoint) {
      res = http.del(`${BASE_URL}${endpoint}`, null, auth);
      check(res, { [`delete ${resource.type} 2xx`]: (r) => is2xx(r.status) || r.status === 404 });
      sleep(0.1);
    }
  }

  sleep(1);
}
