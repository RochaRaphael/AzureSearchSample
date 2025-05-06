# AzureSearchSample

[![.NET 6](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)](https://dotnet.microsoft.com/) [![Azure Cognitive Search](https://img.shields.io/badge/Azure%20Search-Cognitive-blue?logo=microsoftazure)](https://azure.microsoft.com/services/search/) [![Azure OpenAI](https://img.shields.io/badge/Azure%20OpenAI-Embeddings-gray?logo=openai)](https://azure.microsoft.com/services/openai/) [![License: MIT](https://img.shields.io/badge/license-MIT-green)](LICENSE.txt)

**AzureSearchSample** is a demonstration of a semantic search engine using **Azure AI Search** and **Azure OpenAI** embeddings. It shows how to overcome the limitations of traditional keyword search by understanding the *meaning* behind user queries. For example, a keyword search for “comfortable chair” might fail or return irrelevant results. In contrast, this semantic search solution leverages OpenAI to convert text into high-dimensional vectors and Azure AI Search to retrieve the most relevant products (e.g., recliner chairs or upholstered chairs), significantly improving search relevance and user experience.

Part of the portfolio of developer Raphael Rocha, this project highlights the full integration of AI on Azure. It is written in **C# (.NET 8)** with best practices such as dependency injection and configuration via options. The backend is a Web API (which could be hosted as an Azure Function or App Service), and all data and indexes reside on Azure, making the solution fully managed, scalable, and enterprise-ready.

## Architecture

User queries enter through the **.NET Web API** (Application Server) and are sent to **Azure OpenAI** to generate the text embeddings. The **Azure AI Search** index (backed by product or document data) stores the pre-calculated embeddings and other fields. The API performs a vector search (similarity) in Azure AI Search and returns the most relevant results to the user. Azure DevOps can be used to automatically build and deploy the API to Azure.

* **OpenAI (Azure OpenAI Service)** – Generates 1024-dimensional vector embeddings from the query text and document fields (e.g., product name/description) using a model like `text-embedding-3-large`.
* **Azure AI Search (Vector Search)** – Stores indexed documents with their embedding vectors. When it receives a query vector, it uses approximate nearest neighbor (k-NN) search to find the most semantically similar items.
* **.NET 8 Web API** – Written in C#, this service (`SearchAIService`) orchestrates the workflow. It calls the OpenAI embeddings API, formats the results into `Product` objects with vector fields, and sends them to the Azure Search index. It also handles search queries: converts the query into a vector and calls Azure Search's **Vector Search** feature.
* **Azure Functions / App Service** – (Deployment Options) The API can be deployed as a serverless function or a web app. The project includes Swagger/OpenAPI configuration for easy testing and can be integrated into a CI/CD pipeline via **Azure DevOps**.

## Key Technologies

* **C# (.NET 8)** – The primary language and framework for the backend service.
* **Azure AI Search** – Enterprise-grade search as a service, configured with a **Standard** index to support vector fields and semantic ranking.
* **Azure OpenAI Service** – Provides the `text-embedding-3-large` model to generate high-quality text embeddings.
* **Azure Functions / App Service** – (Deployment Options) Can host the API for serverless execution or as a web application.
* **Azure DevOps / CI-CD** – Pipeline tools for continuous integration and deployment of the solution.
* **Azure SDKs** – Uses `Azure.Search.Documents` for search operations and `OpenAI.Embeddings` (Azure OpenAI SDK) for embeddings.

## Setup

### Prerequisites

* **Azure Subscription** – You need an Azure account to create resources.
* **Azure AI Search** – Provision a **Standard** type Search resource in the Azure portal. Create an index (e.g., `products-index`) with the fields: `id` (key), `name`, `image`, `price`, `productSKUs`, and two vector fields (e.g., `productNameVector`, `productDescriptionVector` as `Collection(Edm.Single)`). Set the vector dimensions (1024) and similarity parameters (e.g., cosine) for these fields.
* **Azure OpenAI Service** – Get access (API key and endpoint) for an Azure OpenAI resource with embedding models enabled.
* **.NET 6 SDK** – Install the .NET 6 (or higher) SDK to build and run the C# code.
* **Client Tools** – Git to clone the repository and any HTTP tool (like curl, Postman) to call the API.

### Configuration

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/RochaRaphael/AzureSearchSample.git
   cd AzureSearchSample/Omnion.PesquisaIA.Gateway
   ```

2. **Set API Keys**: In the `Omnion.PesquisaIA.Gateway/appsettings.json` file (or your environment), configure the following under `ApiSettings`:

   * `AzureSearchEndpoint`: URL of your Azure AI Search service (e.g., `https://<your-service>.search.windows.net`).
   * `AzureSearchKey`: Admin key for your Azure Search service.
   * `AzureSearchIndexName`: The name of the index you created (e.g., `products-index`).
   * `OpenAIEndpoint`: Your Azure OpenAI endpoint (e.g., `https://<your-openai-resource>.cognitiveservices.azure.com/`).
   * `OpenAIKey`: Azure OpenAI API key.

   Example `appsettings.json` snippet:

   ```json
   "ApiSettings": {
     "AzureSearchEndpoint": "https://your-search.search.windows.net",
     "AzureSearchKey": "YourSearchAdminKey",
     "AzureSearchIndexName": "products-index",
     "OpenAIEndpoint": "https://your-openai-resource.openai.azure.com/",
     "OpenAIKey": "YourOpenAIApiKey"
   }
   ```

### Running Locally

* **Build and Run**: In a terminal, navigate to `Omnion.PesquisaIA.Gateway` and run:

  ```bash
  dotnet restore
  dotnet run
  ```

  The Web API will start (by default on [http://0.0.0.0:10098](http://0.0.0.0:10098)). You should see the Swagger interface at `http://localhost:10098/swagger` for testing the endpoints.

* **Populate the Index**: Use the `AddProductToAzureSearchIndex` endpoint to submit documents. This endpoint generates embeddings for the product name and description using OpenAI and then indexes the resulting `Product` object in Azure Search.

  Example JSON payload to add a product:

  ```json
  POST /api/SearchAI/AddProductToAzureSearchIndex
  Content-Type: application/json

  {
    "id": "p1",
    "name": "Ergonomic Office Chair",
    "description": "A comfortable chair with lumbar support.",
    "image": "http://example.com/images/chair.jpg",
    "price": 129.99,
    "productSKUs": ["SKU123", "SKU456"]
  }
  ```

* **Run a Semantic Search**: Use the `SearchByVectorSimilarity` endpoint to query the index. Provide the query text as JSON (the field name is `queryText` in the code). The API will convert the query into a vector and perform a vector similarity search in the Azure index, returning the most relevant products.

  Example search request:

  ```json
  POST /api/SearchAI/SearchByVectorSimilarity
  Content-Type: application/json

  {
    "queryText": "comfortable chair"
  }
  ```

  The response will be a list of products (id, name, image, price, SKUs) most relevant to the meaning of the query.

## Key Code Snippets

* **Generating and Indexing Embeddings**: The service uses the Azure OpenAI SDK to generate embeddings for the `name` and `description` fields of the product:

```csharp
var embeddings = await GetOpenAIEmbeddingsAsync(text);
```

* **Searching with Vector Similarity**: The user's query is converted into an embedding vector and compared with embeddings in Azure AI Search:

```csharp
var results = await azureSearchClient.SearchAsync(queryEmbedding);
```

For more details about the development of this project, [access my post about the project](https://medium.com/@rocharaphael0911/building-a-semantic-search-engine-with-azure-search-and-openai-embeddings-with-real-code-c1a2f5cca916).
---



# Português 
<img src="https://upload.wikimedia.org/wikipedia/commons/0/05/Flag_of_Brazil.svg" width="80" />  <img src="https://upload.wikimedia.org/wikipedia/commons/5/5c/Flag_of_Portugal.svg" width="80" />


# AzureSearchSample

[![.NET 6](https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet)](https://dotnet.microsoft.com/) [![Azure Cognitive Search](https://img.shields.io/badge/Azure%20Search-Cognitive-blue?logo=microsoftazure)](https://azure.microsoft.com/services/search/) [![Azure OpenAI](https://img.shields.io/badge/Azure%20OpenAI-Embeddings-gray?logo=openai)](https://azure.microsoft.com/services/openai/) [![License: MIT](https://img.shields.io/badge/license-MIT-green)](LICENSE.txt)

**AzureSearchSample** é uma demonstração de um mecanismo de busca semântico usando **Azure AI Search** e **Azure OpenAI** embeddings. Ele mostra como superar as limitações da busca por palavras-chave tradicionais, compreendendo o *significado* por trás das consultas do usuário. Por exemplo, uma busca por palavra-chave por “cadeira confortável” pode falhar ou retornar resultados irrelevantes. Em contraste, essa solução de busca semântica utiliza o OpenAI para converter texto em vetores de alta dimensão e o Azure AI Search para recuperar os produtos mais relevantes (por exemplo, cadeiras reclináveis ou cadeiras estofadas), melhorando significativamente a relevância da busca e a experiência do usuário.

Parte do portfólio do desenvolvedor Raphael Rocha, este projeto destaca a integração completa de IA no Azure. Ele foi escrito em **C# (.NET 8)** com boas práticas como injeção de dependência e configuração via opções. O backend é uma API Web (que poderia ser hospedada como uma Função do Azure ou App Service), e todos os dados e índices ficam no Azure, tornando a solução totalmente gerenciada, escalável e pronta para empresas.

## Arquitetura
As consultas do usuário entram pela **API Web .NET** (Servidor de Aplicações) e são enviadas para o **Azure OpenAI** para gerar os embeddings do texto. O índice do **Azure AI Search** (suportado por dados de produto ou documentos) armazena os embeddings pré-calculados e outros campos. A API realiza uma busca vetorial (semelhança) no Azure AI Search e retorna os resultados mais correspondentes para o usuário. O Azure DevOps pode ser usado para construir e implantar a API no Azure automaticamente.

* **OpenAI (Azure OpenAI Service)** – Gera embeddings vetoriais de 1024 dimensões a partir do texto da consulta e dos campos dos documentos (por exemplo, nome/descrição do produto) utilizando um modelo como `text-embedding-3-large`.
* **Azure AI Search (Busca Vetorial)** – Armazena os documentos indexados com seus vetores de embedding. Quando recebe um vetor de consulta, ele usa a busca de vizinhos mais próximos aproximados (k-NN) para encontrar os itens mais semanticamente semelhantes.
* **API Web .NET 8** – Escrito em C#, este serviço (`SearchAIService`) orquestra o fluxo de trabalho. Ele chama a API de embeddings do OpenAI, formata os resultados em objetos `Product` com campos vetoriais e os envia para o índice do Azure Search. Ele também lida com as consultas de busca: converte a consulta em um vetor e chama o recurso de **Busca Vetorial** do Azure Search.
* **Azure Functions / App Service** – (Opções de Implantação) A API pode ser implantada como uma função serverless ou aplicativo web. O projeto inclui configuração do Swagger/OpenAPI para testes fáceis e pode ser integrado a um pipeline de CI/CD via **Azure DevOps**.

## Tecnologias Chave

* **C# (.NET 8)** – Linguagem e framework principais para o serviço de backend.
* **Azure AI Search** – Busca como serviço empresarial, configurada com um índice do tipo **Standard** para suportar campos vetoriais e ranqueamento semântico.
* **Azure OpenAI Service** – Fornece o modelo `text-embedding-3-large` para gerar embeddings de texto de alta qualidade.
* **Azure Functions / App Service** – (Opções de Implantação) Pode hospedar a API para execução serverless ou como aplicação web.
* **Azure DevOps / CI-CD** – Ferramentas de pipeline para integração e implantação contínuas da solução.
* **SDKs do Azure** – Usa `Azure.Search.Documents` para operações de busca e `OpenAI.Embeddings` (SDK do Azure OpenAI) para embeddings.

## Configuração

### Pré-requisitos

* **Assinatura do Azure** – Você precisa de uma conta do Azure para criar recursos.
* **Azure AI Search** – Provisione um recurso de Search do tipo *Standard* no portal do Azure. Crie um índice (por exemplo, `products-index`) com os campos: `id` (chave), `name`, `image`, `price`, `productSKUs`, e dois campos vetoriais (por exemplo, `productNameVector`, `productDescriptionVector` como `Collection(Edm.Single)`). Defina as dimensões dos vetores (1024) e os parâmetros de similaridade (por exemplo, cosseno) para esses campos.
* **Azure OpenAI Service** – Obtenha acesso (chave de API e endpoint) para um recurso Azure OpenAI com modelos de embedding habilitados.
* **SDK .NET 6** – Instale o SDK .NET 6 (ou superior) para construir e rodar o código em C#.
* **Ferramentas de Cliente** – Git para clonar o repositório e qualquer ferramenta HTTP (como curl, Postman) para chamar a API.

### Configuração

1. **Clone o Repositório**:

   ```bash
   git clone https://github.com/RochaRaphael/AzureSearchSample.git
   cd AzureSearchSample/Omnion.PesquisaIA.Gateway
   ```

2. **Configurar Chaves de API**: No arquivo `Omnion.PesquisaIA.Gateway/appsettings.json` (ou no seu ambiente), configure o seguinte em `ApiSettings`:

   * `AzureSearchEndpoint`: URL do seu serviço Azure AI Search (por exemplo, `https://<your-service>.search.windows.net`).
   * `AzureSearchKey`: Chave de administrador para seu serviço Azure Search.
   * `AzureSearchIndexName`: O nome do índice que você criou (por exemplo, `products-index`).
   * `OpenAIEndpoint`: Seu endpoint Azure OpenAI (por exemplo, `https://<your-openai-resource>.cognitiveservices.azure.com/`).
   * `OpenAIKey`: Chave de API do Azure OpenAI.

   Exemplo de trecho de `appsettings.json`:

   ```json
   "ApiSettings": {
     "AzureSearchEndpoint": "https://your-search.search.windows.net",
     "AzureSearchKey": "YourSearchAdminKey",
     "AzureSearchIndexName": "products-index",
     "OpenAIEndpoint": "https://your-openai-resource.openai.azure.com/",
     "OpenAIKey": "YourOpenAIApiKey"
   }
   ```

### Executando Localmente

* **Construir e Executar**: Em um terminal, navegue até `Omnion.PesquisaIA.Gateway` e execute:

  ```bash
  dotnet restore
  dotnet run
  ```

  A API Web será iniciada (por padrão, em [http://0.0.0.0:10098](http://0.0.0.0:10098)). Você deve ver a interface Swagger em `http://localhost:10098/swagger` para testar os endpoints.

* **Popular o Índice**: Use o endpoint `AddProductToAzureSearchIndex` para enviar documentos. Esse endpoint gera embeddings para o nome e a descrição do produto usando o OpenAI, e depois indexa o objeto `Product` resultante no Azure Search.

  Exemplo de payload JSON para adicionar um produto:

  ```json
  POST /api/SearchAI/AddProductToAzureSearchIndex
  Content-Type: application/json

  {
    "id": "p1",
    "name": "Cadeira Ergonômica de Escritório",
    "description": "Uma cadeira confortável com suporte lombar.",
    "image": "http://example.com/images/chair.jpg",
    "price": 129.99,
    "productSKUs": ["SKU123", "SKU456"]
  }
  ```

* **Executar uma Busca Semântica**: Use o endpoint `SearchByVectorSimilarity` para consultar o índice. Forneça o texto da consulta como JSON (o nome do campo é `textoConsulta` no código). A API converterá a consulta em um vetor e realizará a busca de similaridade vetorial no índice do Azure, retornando os produtos mais relevantes.

  Exemplo de requisição de busca:

  ```json
  POST /api/SearchAI/SearchByVectorSimilarity
  Content-Type: application/json

  {
    "textoConsulta": "cadeira confortável"
  }
  ```

  A resposta será uma lista de produtos (id, nome, imagem, preço, SKUs) mais relevantes para o significado da consulta.

## Principais Trechos de Código

* **Gerando e Indexando Embeddings**: O serviço usa o


SDK Azure OpenAI para gerar embeddings para os campos `name` e `description` do produto:

```csharp
var embeddings = await GetOpenAIEmbeddingsAsync(text);
```

* **Buscando com Similaridade Vetorial**: A consulta do usuário é convertida em um vetor de embeddings e comparada com os embeddings no Azure AI Search:

  ```csharp
  var results = await azureSearchClient.SearchAsync(queryEmbedding);
  ```

Para mais detalhes sobre o desenvolvimento deste projeto, acesse [meu post sobre o projeto](https://medium.com/@rocharaphael0911/criando-um-motor-de-busca-sem%C3%A2ntico-com-azure-search-e-embeddings-da-openai-com-c%C3%B3digo-real-d82f86160e40).
---
