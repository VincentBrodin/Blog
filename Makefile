build: ## Builds the docker image and replaces the old one with the new one.
	make tailwind 
	docker build -f Blog.Api/Dockerfile -t blogapi:latest --no-cache .

stop:
	-docker stop blog

deploy: ## Stops, removes and creates container.
	make stop
	-docker rm blog
	docker run -d -p 5068:8080 -p 5069:8081 --volume blogs:/app/data --name blog blogapi:latest

up: ## Builds and deploys docker container.
	make build
	make deploy

debug: ## Runs project in debug mode and HTTP.
	make tailwind 
	-dotnet run --project Blog.Api -c Debug --start-profile "HTTP"


release: ## Runs project in release mode and HTTP.
	make tailwind 
	-dotnet run --project Blog.Api -c Release --start-profile "HTTP"

tailwind: ## Builds tailwind
	-npm --prefix ./Blog.Api  run css:build

.PHONY: help
help: ## Display this help message.
	@echo Available targets:
	@findstr /R /C:"^[a-zA-Z0-9_-][a-zA-Z0-9_-]*:.*##" $(MAKEFILE_LIST) | sort

