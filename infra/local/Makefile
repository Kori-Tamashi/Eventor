include settings.conf

ENV_FILE := settings.conf
DOCKER_COMPOSE_FILE := docker/docker-compose.yml

all:
	@echo "$(POSTGRES_DB)"  

database:
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) up -d database

database-interface:
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) up -d database-interface

web-server:
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) up -d web-server

backend:
	docker compose --env-file $(ENV_FILE) -f $(DOCKER_COMPOSE_FILE) up -d backend