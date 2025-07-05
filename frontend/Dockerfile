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

# new: entrypoint script reinlegen
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
