Feature: Manual Trigger Endpoints

Scenario: Manually trigger expired asset content cleanup
    Given I am authenticated as a user
    When I trigger the expired asset content cleanup
    Then the response status code should be 200

Scenario: Manually trigger expired asset metadata cleanup
    Given I am authenticated as a user
    When I trigger the expired asset metadata cleanup
    Then the response status code should be 200