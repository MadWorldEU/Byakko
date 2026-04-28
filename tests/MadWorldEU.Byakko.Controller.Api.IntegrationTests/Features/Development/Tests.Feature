Feature: Api Tests Endpoints

Scenario: Ping endpoint returns pong
    When I send a GET request to "/tests/ping"
    Then the response status code should be 200
    And the response body should be "pong"
