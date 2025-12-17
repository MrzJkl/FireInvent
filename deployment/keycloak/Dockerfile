FROM quay.io/keycloak/keycloak:26.4.7 AS builder

ENV KC_DB=postgres

WORKDIR /opt/keycloak

# Build server
RUN /opt/keycloak/bin/kc.sh build

# Use the final Keycloak image
FROM quay.io/keycloak/keycloak:26.4.7

# Copy built Keycloak instance
COPY --from=builder --chown=keycloak:keycloak /opt/keycloak/ /opt/keycloak/

ENV KC_DB=postgres

# Define entrypoint
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]
