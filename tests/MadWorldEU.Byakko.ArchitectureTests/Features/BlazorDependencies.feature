Feature: Blazor Dependency Rules

Scenario: Blazor.Shared should not depend on server-side layers
    Given the architecture is loaded
    Then the BlazorShared layer should not depend on the Domain layer
    And the BlazorShared layer should not depend on the Application layer
    And the BlazorShared layer should not depend on the Postgresql layer
    And the BlazorShared layer should not depend on the ObjectStorage layer
    And the BlazorShared layer should not depend on the Security layer
    And the BlazorShared layer should not depend on the Mail layer
    And the BlazorShared layer should not depend on the Api layer

Scenario: Admin should not depend on server-side layers
    Given the architecture is loaded
    Then the Admin layer should not depend on the Domain layer
    And the Admin layer should not depend on the Application layer
    And the Admin layer should not depend on the Postgresql layer
    And the Admin layer should not depend on the ObjectStorage layer
    And the Admin layer should not depend on the Security layer
    And the Admin layer should not depend on the Mail layer
    And the Admin layer should not depend on the Api layer
    And the Admin layer should not depend on the Status layer

Scenario: Portal should not depend on server-side layers
    Given the architecture is loaded
    Then the Portal layer should not depend on the Domain layer
    And the Portal layer should not depend on the Application layer
    And the Portal layer should not depend on the Postgresql layer
    And the Portal layer should not depend on the ObjectStorage layer
    And the Portal layer should not depend on the Security layer
    And the Portal layer should not depend on the Mail layer
    And the Portal layer should not depend on the Api layer
    And the Portal layer should not depend on the Status layer