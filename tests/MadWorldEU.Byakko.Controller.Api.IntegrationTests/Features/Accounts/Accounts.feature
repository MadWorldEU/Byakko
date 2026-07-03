Feature: Account Endpoints

Scenario: Create my account as a user
    Given I am authenticated as a user
    When I create my account
    Then the response status code should be 201
    And the created account should be returned

Scenario: Request deletion of my account as a user
    Given I am authenticated as a user
    And I have created my account
    When I request deletion of my account
    Then the response status code should be 200
    And the deletion request account should be returned
    When I request my account
    Then the response status code should be 200
    And the account should have deletion requested

Scenario: Retrieve my account as a user
    Given I am authenticated as a user
    And I have created my account
    When I request my account
    Then the response status code should be 200
    And the account should be returned with no deletion requested