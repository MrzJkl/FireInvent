# --- Build Stage ---
FROM dart:stable AS build

WORKDIR /app
COPY . .

# Install Flutter SDK
RUN git clone https://github.com/flutter/flutter.git -b stable /flutter \
  && /flutter/bin/flutter doctor

ENV PATH="/flutter/bin:/flutter/bin/cache/dart-sdk/bin:${PATH}"

# Enable web
RUN flutter config --enable-web

# Fetch dependencies & build
RUN flutter pub get
RUN flutter build web --release

# --- Serve Stage ---
FROM nginx:alpine

# Copy built web app
COPY --from=build /app/build/web /usr/share/nginx/html

# Optional: custom nginx config (e.g., for fallback to index.html for routing)
COPY nginx.conf /etc/nginx/conf.d/default.conf
