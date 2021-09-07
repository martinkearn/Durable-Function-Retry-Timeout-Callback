# Durable Function Retry Timeout and Call-back
A Durable Function example showing how to call external services with a call-back, retry and timeout. 

The sample uses a Durable Entity to track the status and output of attempts to call and external API.

## Components

- **Domain**: Domain porject which contains models that are shared across other projects.
- **FunctionApp**: This is a Durable Function project which contains the main orchestration function, its clients, activities and entities. The functions included are as follows:
  - `FunctionApp.Clients.HttpStartClient` This is the main http client which can be requested to start the main orchestration function
  - `FunctionApp.Clients.HttpAttemptCounterEntityClient` This is a http client function which can be used to get the latest attempt data (`AttemptsEntityState`)
  - `FunctionApp.Orchestrators.MainOrchestrator` The main orchestration function which carries out the requests and retries to the API and waits for the external system call-back.
  - `FunctionApp.Activities.CallApiActivity` the activity function which makes the actual calls to the Api.
  - `FunctionApp.Entities.AttemptsEntity` the entity function which stores data abojut attempts to call the Api.
- **WebApi**: This is a web api project which contains a very simple API which the function will attempt to call and re-attempt based on the settings. This API represents an external system which the function needs to call.

## Usage
You will need [Visual Studio](https://visualstudio.microsoft.com/) and [Postman](https://www.postman.com/) (or alternatives) to run the solution.

The sample is designed to be run in Visual Studio 2022 (Visual Studio 2019 has almost identical steps) and PostMan. If you are using an alternative editor or API tool, you may need to adapt some of these steps for your toolset.

#### Run the solution

To run the sample you will need to run both the `FunctionApp` and `WebApi` concurrently, to configure this:

1. Right-click on the `RetryTimeoutCallback` solution and choose `Set Startup porjects`
2. Choose `Multiple startup projects`
3. Set both `FunctionApp` and `WebApi` to "Start"
4. Start the solution

#### Invoke the Orchestration

When the solution runs, you will see a `func.exe` console which will list out all the function in the solution. We will make a request to the `HttpStartClient` to start the main orchestration.

1. From the `func.exe` console, copy the URL for `HttpStartClient`. It is typically something like "http://localhost:7071/api/HttpStartClient"
2. Make a new `POST` request in Postman to the URL obtained in step 1 (no body required)
3. To confirm that the function is running, make a new request to the URL in the `statusQueryGetUri` property in the response body in step 2

#### Observing Attempts

You can observe the attempts to the API via the HttpAttemptCounterEntityClient function. 

If you leave the function to run without intervention it will make 5 attempts, all of which will end with the state of "TimedOut" after 60 seconds. Each attempt status will go through the following sequential states if you do not make the call back manually:

1. New
2. Executing
3. ExecutedSuccess
4. WaitingForCallback
5. TimedOut

You can repeat the request in step 3 multiple times and see the attempts data expand over time.

1. Follow the steps in "Invoke the Orchestration" and make a copy of the `id` property from the response body in step 2.
2. From the `func.exe` console, copy the URL for `HttpAttemptCounterEntityClient`. It is typically something like "http://localhost:7071/api/HttpAttemptCounterEntityClient"
3. Make a new `GET` request in Postman to the URL obtained in step 2 (no body required) but append `?instanceid={Id}` where `id` is the value you obtained in step 1. The full request url will be something like "http://localhost:7071/api/HttpAttemptCounterEntityClient?instanceid=13a990ff1f914a13995a11ae1ff1fc3c"
4. Repeat step 3 until the `overallState` is "Completed API request after 5 attempts. Final state TimedOut, status text: Event Callback not received in 00:01:00"

#### Making the Call-back

The system is designed to expect a call-back from an external system. In this case, the external system is Postman, but this could be an external system like Databricks, Logic Apps etc.

To make the call back and allow the function to complete with success, follow these steps:

1. Follow the steps in "Invoke the Orchestration" and make a copy of the `id` property from the response body in step 2.
2. Copy the URL for `sendEventPostUri` property from the response body. It is typically something like "http://localhost:7071/runtime/webhooks/durabletask/instances/3f733347b1cd44d1af699e8993a68b7f/raiseEvent/{eventName}?taskHub=TestHubName&connection=Storage&code=MaHpwEx2o6sEZKMoDAG8dfWihFNm7Pa1DxdcNuQRlXr7PivLT/9rlA=="
3. Replace `{eventname}` with `Callback`
4. Make a `POST` request to the url you created in step 3. The body should be a raw json body with just the word "true" (no json structure). You should get a `202/Accepted` response
5. If you make a request to `HttpAttemptCounterEntityClient` (see step 3 of Observing Attempts) you should see that a single attempt was made which resulted in "CallbackSuccess"
6. If you make a request to `statusQueryGetUri` (see step 3 of Invoke the Orchestration) you should see that the orchestration function completed with a `runtimeStatus` of "Completed" and an output hat reads something like "Completed API request after 1 attempts. Final state CallbackSuccess, status text: External system called back with CallbackSuccess"
