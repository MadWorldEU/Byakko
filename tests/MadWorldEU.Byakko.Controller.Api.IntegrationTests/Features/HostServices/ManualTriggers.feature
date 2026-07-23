Feature: Manual Trigger Endpoints

Scenario: Manually trigger expired asset content cleanup
    Given I am authenticated as a user
    And I have set up an expired asset with uploaded content
    When I trigger the expired asset content cleanup
    Then the response status code should be 200
    And the expired asset content should be marked as deleted

Scenario: Manually trigger expired asset metadata cleanup
    Given I am authenticated as a user
    And I have set up a soft-deleted asset
    When I trigger the expired asset metadata cleanup
    Then the response status code should be 200
    And the soft-deleted asset should be permanently removed

Scenario: Manually trigger expired asset metadata cleanup removes associated audit logs
    Given I am authenticated as an administrator
    And I have set up a soft-deleted asset
    When I trigger the expired asset metadata cleanup
    Then the response status code should be 200
    And the audit logs for the soft-deleted asset should be removed

    Scenario: Manually trigger account deletion cleanup
        Given I am authenticated as a user
        And I have created my account
        And I have registered a user in the authentication server
        And I have created an asset for the account
        And I have requested deletion of my account
        Given I am authenticated as an administrator
        And I have confirmed the deletion of the account
        When I trigger the account deletion cleanup
        Then the response status code should be 200
        And the account should have status Deleted
        And the asset should be permanently deleted
        And the user should be deleted from the authentication server