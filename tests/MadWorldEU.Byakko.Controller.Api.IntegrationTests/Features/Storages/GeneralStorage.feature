Feature: General Storage Endpoints

Scenario: Retrieve storage statistics as an administrator
    Given I am authenticated as an administrator
    And I have created an asset with name "stats-test.txt" and content type "text/plain"
    And I have uploaded content with name "stats-test.txt" for the created asset
    When I request the storage statistics
    Then the response status code should be 200
    And the storage statistics total files should be at least 1
    And the storage statistics total bytes should be greater than zero