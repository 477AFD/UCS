# 03-final-validation: Validate the scoped upgrade

Validate the complete project-scoped upgrade after the atomic migration. Build the upgraded project and the solution only as needed to verify that the changed project does not regress solution integration, run discovered tests where applicable, inspect warnings, and document any runtime/UI checks that require user verification because WinForms execution is environment-dependent.

**Done when**: the upgraded project builds with zero errors and zero warnings, applicable tests pass, only the specified project and required workflow artifacts/dependencies were changed, and remaining manual verification items are documented clearly.
