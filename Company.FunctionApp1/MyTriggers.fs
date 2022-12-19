namespace Company.FunctionApp1.MyTriggers

open System.Threading.Tasks
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.DurableTask
open Microsoft.Extensions.Logging

[<AbstractClass; Sealed>]
type MyTriggers private () =
    
    
    [<Function("StringActivity")>]
    static member StringActivity
        ([<ActivityTrigger>] msg: string)
        (ctx: FunctionContext)
        : string
        =
        let log = ctx.GetLogger("stringActivity")
        log.LogInformation("msg received {msg}", msg)
        
        $"return msg {msg}"

    [<Function("MyOrchestration")>]
    static member MyOrchestration
        ([<OrchestrationTrigger>] ctx: TaskOrchestrationContext)
        : Task<string>
        =
        let msg = ctx.GetInput<string>()
        ctx.CallActivityAsync<string>("StringActivity", msg)
        
        
    [<Function("HttpTestOrchestration")>]
    static member HttpTestOrchestration (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test-orchestration")>]req: HttpRequestData,
        ctx: FunctionContext,
        [<DurableClient>] starter: DurableClientContext
        )
        : Task<HttpResponseData>
        =
        task {
            let! instanceId = starter.Client.ScheduleNewOrchestrationInstanceAsync("MyOrchestration", "test msg")
            return starter.CreateCheckStatusResponse(req, instanceId)
        }