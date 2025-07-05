#!/bin/sh

echo "API_URL is set to: ${API_URL}"
echo "window.env = { API_URL: \"${API_URL}\" };" > /usr/share/nginx/html/env.js

exec "$@"