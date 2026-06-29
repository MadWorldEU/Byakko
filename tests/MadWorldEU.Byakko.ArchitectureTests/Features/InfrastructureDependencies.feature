Feature: Infrastructure Dependency Rules

Scenario: Application layer should not depend on Infrastructure or Controllers
    Given the architecture is loaded
    Then the Application layer should not depend on the Postgresql layer
    And the Application layer should not depend on the ObjectStorage layer
    And the Application layer should not depend on the Security layer
    And the Application layer should not depend on the Api layer
    And the Application layer should not depend on the Status layer

Scenario: Postgresql should not depend on Controllers
    Given the architecture is loaded
    Then the Postgresql layer should not depend on the Api layer
    Then the Postgresql layer should not depend on the Status layer

Scenario: ObjectStorage should not depend on Controllers
    Given the architecture is loaded
    Then the ObjectStorage layer should not depend on the Api layer
    Then the ObjectStorage layer should not depend on the Status layer

Scenario: Security should not depend on Controllers
    Given the architecture is loaded
    Then the Security layer should not depend on the Api layer
    Then the Security layer should not depend on the Status layer

Scenario: Mail should not depend on Controllers
    Given the architecture is loaded
    Then the Mail layer should not depend on the Api layer
    Then the Mail layer should not depend on the Status layer

Scenario: Application layer should not depend on Mail
    Given the architecture is loaded
    Then the Application layer should not depend on the Mail layer