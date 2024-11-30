module ProcessBlobCreated

open Microsoft.Azure.WebJobs
open Microsoft.Extensions.Logging
open Newtonsoft.Json.Linq

type EventGridEvent = {
    [<JsonProperty("data")>] Data: JObject
    [<JsonProperty("eventType")>] EventType: string
}

[<FunctionName("ProcessBlobCreated")>]
let run([<EventGridTrigger>] eventGridEvent: JObject, log: ILogger) =
    log.LogInformation($"Received EventGrid event: {eventGridEvent}")

    // Check for subscription validation event
    let eventType = eventGridEvent.["eventType"]?.ToString()
    if eventType = "Microsoft.EventGrid.SubscriptionValidationEvent" then
        let validationCode = eventGridEvent.["data"]?["validationCode"]?.ToString()
        let response = JObject()
        response.Add("validationResponse", JValue(validationCode))
        log.LogInformation($"Responding to subscription validation with code: {validationCode}")
        return response
    else
        log.LogInformation($"Processing other EventGrid event type: {eventType}")
