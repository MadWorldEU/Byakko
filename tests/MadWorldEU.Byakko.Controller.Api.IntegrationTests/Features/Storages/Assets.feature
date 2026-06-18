Feature: Asset Storage Endpoints

Scenario: Retrieve two assets as an administrator
    Given I am authenticated as an administrator
    And I have created 2 assets
    When I request page 1 of all assets
    Then the response status code should be 200
    And the response should contain the created assets

Scenario: Delete content of an asset as an administrator
    Given I am authenticated as an administrator
    And I have created an asset with name "delete-test.txt" and content type "text/plain"
    And I have uploaded content for the created asset
    When I delete the content of the created asset
    Then the response status code should be 200
    When I retrieve the metadata of the created asset
    Then the response status code should be 200
    And the expire date of the created asset should be in the past

Scenario: Retrieve upload limits for an authenticated user
    Given I am authenticated as a user
    And I have created an asset with name "limits-test.txt" and content type "text/plain"
    When I request my upload limits
    Then the response status code should be 200
    And the upload limits should reflect my usage

Scenario: Retrieve my own assets as an authenticated user
    Given I am authenticated as a user
    And I have created 2 assets
    When I request page 1 of my assets
    Then the response status code should be 200
    And the response should contain the created assets

Scenario: Delete content of my own asset as an authenticated user
    Given I am authenticated as a user
    And I have created an asset with name "delete-test.txt" and content type "text/plain"
    And I have uploaded content for the created asset
    When I delete my own asset content
    Then the response status code should be 200
    When I retrieve the metadata of the created asset
    Then the response status code should be 200
    And the expire date of the created asset should be in the past

Scenario: Upload and download a file for an asset
    Given I am authenticated as a user
    And I have created an asset with name "test-file.txt" and content type "text/plain"
    When I upload content for the created asset
    Then the response status code should be 200
    When I download the content of the created asset
    Then the response status code should be 200
    And the response body should be "Hello, World!"

Scenario: Upload and download a password-protected file for an asset
    Given I am authenticated as a user
    And I have created an asset with name "secret-file.txt" and content type "text/plain"
    When I upload content for the created asset with password "my-secret-password"
    Then the response status code should be 200
    When I download the content of the created asset with password "my-secret-password"
    Then the response status code should be 200
    And the response body should be "Hello, World!"