# 1. Pega o binário oficial diretamente da imagem do Garage (sem depender de internet/curl)
FROM dxflrs/garage:v1.0.1 AS garage-bin

# 2. Usa Alpine para ter shell e ferramentas
FROM alpine:3.20

RUN apk add --no-cache bash

COPY --from=garage-bin /garage /garage
COPY garage.toml /etc/garage.toml
COPY init.sh /init.sh
RUN chmod +x /init.sh /garage

ENTRYPOINT ["/bin/sh", "/init.sh"]
