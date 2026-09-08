# Multi-stage Dockerfile for AAQSOLS Heart Clinic Web UI
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files for dependency caching
COPY package*.json ./
RUN npm install

# Copy source and build
COPY . .
RUN npm run build

# Nginx runtime
FROM nginx:alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
