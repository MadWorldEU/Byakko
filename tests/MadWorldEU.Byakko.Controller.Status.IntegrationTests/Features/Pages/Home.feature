Feature: Home Page

Scenario: Home page loads successfully
    When I send a GET request to "/"
    Then the response status code should be 200

Scenario: Home page shows system status heading
    When I send a GET request to "/"
    Then the response status code should be 200
    And the response body should contain "System Status"

Scenario: Health endpoint returns healthy
    When I send a GET request to "/health"
    Then the response status code should be 200
    And the response body should be "Healthy"