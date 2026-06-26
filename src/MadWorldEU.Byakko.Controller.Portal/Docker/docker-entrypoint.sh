#!/bin/sh
set -e
envsubst '${OG_IMAGE_URL}' < /usr/share/nginx/html/index.html > /tmp/index.html \
    && mv /tmp/index.html /usr/share/nginx/html/index.html
exec nginx -g 'daemon off;'