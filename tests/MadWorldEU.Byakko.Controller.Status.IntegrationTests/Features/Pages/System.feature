Feature: System Page

Scenario: System page loads successfully
    When I send a GET request to "/system"
    Then the response status code should be 200
    And the response body should contain "System Info"

Scenario: System page shows the image tag
    When I send a GET request to "/system"
    Then the response status code should be 200
    And the response body should contain "v1.0.0"

Scenario: System page contains a link to the GitHub release
    When I send a GET request to "/system"
    Then the response status code should be 200
    And the response body should contain "https://github.com/MadWorldEU/Byakko/releases/tag/v1.0.0"