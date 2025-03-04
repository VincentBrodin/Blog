build: ## Builds the docker image and replaces the old one with the new one.
	make tailwind 
	docker build -f Blog.Api/Dockerfile -t vincentbrodin/blogapi:latest --no-cache .

stop:
	-docker stop blog

deploy: ## Stops, removes and creates container.
	make stop
	-docker rm blog
	docker run -d -p 5050:8080 -p 5051:8081 --volume blogs:/app/data --name blog vincentbrodin/blogapi:latest

up: ## Builds and deploys docker container.
	make build
	make deploy

push: ## Pushes image to docker hub
	make build
	docker push vincentbrodin/blogapi:latest

debug: ## Runs project in debug mode and HTTP.
	make tailwind 
	-dotnet run --project Blog.Api -c Debug --start-profile "https"


release: ## Runs project in release mode and HTTP.
	make tailwind 
	-dotnet run --project Blog.Api -c Release --start-profile "https"

tailwind: ## Builds tailwind
	-npm --prefix ./Blog.Api  run css:build

.PHONY: help
help: ## Display this help message.
	@echo Available targets:
	@findstr /R /C:"^[a-zA-Z0-9_-][a-zA-Z0-9_-]*:.*##" $(MAKEFILE_LIST) | sort

