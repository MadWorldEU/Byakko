Feature: Send Feedback Endpoint

Scenario: Send feedback as an authenticated user
    Given I am authenticated as a user
    When I send feedback with email "user@example.com" and message "I love this product!"
    Then the feedback request should succeed
    And the administrator should have received an email with subject "Byakko - New feedback"