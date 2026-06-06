Feature: Asset Storage Endpoints

Scenario: Retrieve two assets as an administrator
    Given I am authenticated as an administrator
    And I have created 2 assets
    When I request page 1 of all assets
    Then the response status code should be 200
    And the response should contain the created assets

Scenario: Upload and download a file for an asset
    Given I am authenticated as a user
    And I have created an asset with name "test-file.txt" and content type "text/plain"
    When I upload content for the created asset
    Then the response status code should be 200
    When I download the content of the created asset
    Then the response status code should be 200
    And the response body should be "Hello, World!"