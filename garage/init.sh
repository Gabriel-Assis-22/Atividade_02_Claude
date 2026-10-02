#!/bin/sh
set -e

echo "[garage-init] Aguardando o serviço Garage iniciar..."
until /garage status > /dev/null 2>&1; do
  sleep 1
done

echo "[garage-init] Garage online! Verificando nó..."
NODE_ID=$(/garage status 2>/dev/null | grep -oE '[0-9a-f]{16}' | head -n 1)

if [ -z "$NODE_ID" ]; then
  echo "[garage-init] Tentando obter Node ID via garage node id..."
  NODE_ID=$(/garage node id 2>/dev/null | grep -oE '[0-9a-f]{16}' | head -n 1 || true)
fi

if [ -z "$NODE_ID" ]; then
  echo "[garage-init] Falha ao capturar o Node ID do Garage."
  exit 1
fi

echo "[garage-init] Node ID identificado: $NODE_ID"

# 1. Configurar Layout se ainda não estiver configurado
LAYOUT_STATUS=$(/garage layout show 2>/dev/null || true)
if echo "$LAYOUT_STATUS" | grep -q "No layout configured"; then
  echo "[garage-init] Configurando layout inicial de capacidade..."
  /garage layout assign -z dc1 -c 1G "$NODE_ID"
  /garage layout apply --version 1
  echo "[garage-init] Layout aplicado com sucesso!"
else
  echo "[garage-init] Layout já configurado."
fi

# 2. Importar chave de acesso se ainda não existir
KEY_NAME="app-key"
if ! /garage key list | grep -q "$GARAGE_ACCESS_KEY"; then
  echo "[garage-init] Importando chave de acesso S3..."
  /garage key import "$GARAGE_ACCESS_KEY" "$GARAGE_SECRET_KEY" -n "$KEY_NAME" --yes
  echo "[garage-init] Chave importada com sucesso!"
else
  echo "[garage-init] Chave de acesso já registrada."
fi

# 3. Criar Bucket se não existir
if ! /garage bucket list | grep -q "$GARAGE_BUCKET_NAME"; then
  echo "[garage-init] Criando bucket '$GARAGE_BUCKET_NAME'..."
  /garage bucket create "$GARAGE_BUCKET_NAME"
  echo "[garage-init] Bucket criado com sucesso!"
else
  echo "[garage-init] Bucket '$GARAGE_BUCKET_NAME' já existe."
fi

# 4. Vincular permissões da chave ao bucket
echo "[garage-init] Garantindo permissões de leitura e escrita para '$KEY_NAME' no bucket '$GARAGE_BUCKET_NAME'..."
/garage bucket allow "$GARAGE_BUCKET_NAME" --read --write --key "$KEY_NAME"

# 5. Habilitar website no bucket para visualização pública via s3_web
echo "[garage-init] Habilitando website no bucket '$GARAGE_BUCKET_NAME'..."
/garage bucket website --allow "$GARAGE_BUCKET_NAME" || true

echo "[garage-init] Inicialização do Garage concluída com sucesso!"
