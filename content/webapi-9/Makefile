APP = src/Company.Project.WebAPI
INFRA_PROJECT = src/Company.Project.Infrastructure
ASPIRE_HOST = aspire/Company.Project.AppHost

build:
	dotnet build Company.Project.slnx --tl

watch:
	dotnet watch --project $(APP)

run:
	dotnet run --project $(APP)

ef-add:
	dotnet ef migrations add $(name) -p $(INFRA_PROJECT) -s $(APP) -o  Data/Migrations

ef-remove:
	dotnet ef migrations remove -p $(INFRA_PROJECT) -s $(APP) 

ef-update:
	dotnet ef database update -p  $(INFRA_PROJECT) -s $(APP)