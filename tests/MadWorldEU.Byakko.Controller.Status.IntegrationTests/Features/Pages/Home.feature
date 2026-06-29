Feature: Home Page

Scenario: Home page loads successfully
    When I send a GET request to "/"
    Then the response status code should be 200
    And the response body should contain "System Status"

Scenario: Health endpoint returns healthy
    When I send a GET request to "/health"
    Then the response status code should be 200
    And the response body should be "Healthy"

Scenario Outline: Each service block shows healthy status
    When I send a GET request to "/"
    Then the service "<name>" should have status "Healthy"

    Examples:
        | name           |
        | API            |
        | Portal         |
        | Admin          |
        | Database       |
        | Mail           |
        | Object Storage |
        | Authentication |