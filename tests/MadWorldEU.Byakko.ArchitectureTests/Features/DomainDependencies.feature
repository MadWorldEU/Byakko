Feature: Domain Dependency Rules

Scenario: BuildingBlocks should not depend on any other layer
    Given the architecture is loaded
    Then the BuildingBlocks layer should not depend on the Domain layer
    And the BuildingBlocks layer should not depend on the Application layer
    And the BuildingBlocks layer should not depend on the Contracts layer
    And the BuildingBlocks layer should not depend on the Postgresql layer
    And the BuildingBlocks layer should not depend on the Mail layer
    And the BuildingBlocks layer should not depend on the Api layer
    And the BuildingBlocks layer should not depend on the Status layer

Scenario: Domain layer should only depend on BuildingBlocks
    Given the architecture is loaded
    Then the Domain layer should not depend on the Application layer
    And the Domain layer should not depend on the Contracts layer
    And the Domain layer should not depend on the Postgresql layer
    And the Domain layer should not depend on the Mail layer
    And the Domain layer should not depend on the Api layer
    And the Domain layer should not depend on the Status layer

Scenario: Contracts layer should not depend on Application, Infrastructure or Controllers
    Given the architecture is loaded
    Then the Contracts layer should not depend on the Application layer
    And the Contracts layer should not depend on the Postgresql layer
    And the Contracts layer should not depend on the Mail layer
    And the Contracts layer should not depend on the Api layer
    And the Contracts layer should not depend on the Status layer