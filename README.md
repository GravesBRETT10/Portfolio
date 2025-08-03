
# Brett Graves — ASP.NET Core MVC Portfolio (.NET 8)
Full portfolio with Tailwind styling, API Playground, and a Resume-aware RAG chatbot.

## Run in Visual Studio 2022
1. Open the solution `BrettGravesPortfolio.sln`.
2. Right-click the project -> Manage NuGet Packages -> Restore (or Build once).
3. Build Tailwind:
   - Open **Terminal** at project root and run:
     ```bash
     npm install
     npm run build
     ```
   - This generates `wwwroot/css/site.css`.
4. Set environment variables (PowerShell recommended) **before running**:
   ```powershell
   # Option A: OpenAI
   setx OPENAI_API_KEY "sk-..."
   setx OPENAI_MODEL "gpt-4o-mini"
   setx OPENAI_EMBEDDING_MODEL "text-embedding-3-small"

   # Option B: Azure OpenAI
   setx AZURE_OPENAI_ENDPOINT "https://<your-resource>.openai.azure.com"
   setx AZURE_OPENAI_API_KEY "<key>"
   setx AZURE_OPENAI_CHAT_DEPLOYMENT "<chat-deployment>"
   setx AZURE_OPENAI_EMBEDDING_DEPLOYMENT "<embedding-deployment>"
   ```
5. Ensure the PDFs exist at `wwwroot/files/BrettGraves.pdf` and `wwwroot/files/CoverLetter.pdf` (already included here). On first run the hosted service ingests them to `App_Data/embeddings.db`.
6. Press **F5** to run.

## Pages
- Home, Projects, Skills & Impact, API Playground, Azure & DevOps, AI & Automation, SQL & Data, Resume, Cover Letter, Chat with Brett, Contact

## API Endpoints
- `POST /api/echo` — echoes JSON.
- `GET /api/github?user=<username>` — server-side proxy for GitHub public repos (used by Playground).
- `POST /api/chat` — Resume-aware chat (RAG).
