FROM node:alpine as build

WORKDIR /src

COPY package.json .
RUN npm install

# webpack build
COPY . .

#point the server at our api container port
RUN rm .env
RUN echo "VUE_APP_API_URL='http://localhost:5000'" > .env

RUN npm run build


FROM nginx:alpine

COPY nginx.conf /etc/nginx/conf.d/default.conf

WORKDIR /usr/share/nginx/html
COPY --from=build /src/dist .

# nginx already has `CMD [...]`
