APP = src/GreatIdeas.Template.WebAPI
INFRA_PROJECT = src/GreatIdeas.Template.Infrastructure


build:
	dotnet build --tl

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