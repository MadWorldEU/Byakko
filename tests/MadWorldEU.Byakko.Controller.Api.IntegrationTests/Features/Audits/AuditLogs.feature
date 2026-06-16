Feature: Audit Log Endpoints

Scenario: Retrieve audit logs for an asset as an administrator
    Given I am authenticated as an administrator
    And I have created an asset with name "audit-test.txt" and content type "text/plain"
    When I request the audit logs for the created asset
    Then the response status code should be 200
    And the audit logs should contain a "Created" entry