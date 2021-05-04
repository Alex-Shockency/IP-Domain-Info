FROM node:latest As builder

RUN mkdir -p app
WORKDIR /app
COPY package.json /app
RUN npm install
COPY . /app
RUN npm run build --prod

FROM nginx

COPY --from=builder /app/dist/IP-Domain /usr/share/nginx/html