Feature: Account Endpoints

Scenario: Retrieve accounts with deletion requested as an administrator
    Given I am authenticated as a user
    And I have created my account
    And I have requested deletion of my account
    Given I am authenticated as an administrator
    When I request the accounts with deletion requested for page 1
    Then the response status code should be 200
    And the response should contain at least one account with deletion requested

Scenario: Cancel deletion request of an account as an administrator
    Given I am authenticated as a user
    And I have created my account
    And I have requested deletion of my account
    Given I am authenticated as an administrator
    When I cancel the deletion request of the account
    Then the response status code should be 200
    And the cancel deletion request response should be returned
    When I request my account
    Then the response status code should be 200
    And the account should be returned with no deletion requested

Scenario: Confirm deletion of an account as an administrator
    Given I am authenticated as a user
    And I have created my account
    And I have requested deletion of my account
    Given I am authenticated as an administrator
    When I confirm the deletion of the account
    Then the response status code should be 200
    And the confirm deletion response should be returned
    And the account should have status DeletionConfirmed

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