FROM node:20-alpine

WORKDIR /app

# Copia dependências primeiro (camada de cache)
COPY package*.json ./
RUN npm install --production

# Copia o restante do código
COPY . .

EXPOSE 3000

CMD ["node", "server.js"]
