# Contributing to RestApiGenerator

We welcome and appreciate contributions to the `RestApiGenerator` project! Your efforts can help improve this tool for everyone. Whether it's a bug fix, a new feature, or an improvement to the documentation, your input is valuable.

To ensure a smooth and effective contribution process, please follow these guidelines:

## How to Contribute

1.  **Fork the Repository**: Start by forking the `RestApiGenerator` repository on GitHub. This creates a copy of the project under your GitHub account where you can make your changes.

2.  **Create a New Branch**: Before making any changes, create a new branch for your feature or bug fix. Use a descriptive name for your branch (e.g., `feature/add-yaml-support`, `bugfix/cli-error-handling`).

    ```bash
    git checkout -b your-branch-name
    ```

3.  **Make Your Changes**: Implement your feature or bug fix. Ensure your code adheres to the existing coding style and conventions used in the project.

    *   **Code Style**: Maintain consistency with the current C# coding style (e.g., naming conventions, indentation, brace style).
    *   **Clarity and Readability**: Write clear, concise, and well-commented code.
    *   **Modularity**: Break down complex changes into smaller, manageable functions or classes.

4.  **Ensure Tests Pass**:
    *   If you're fixing a bug, consider adding a new test case that reproduces the bug and then verifies the fix.
    *   If you're adding a new feature, write comprehensive unit and/or integration tests to cover its functionality.
    *   Run all existing tests to ensure your changes haven't introduced any regressions.

    ```bash
    dotnet test
    ```

5.  **Commit Your Changes**: Commit your changes with clear and concise commit messages. A good commit message explains *what* was changed and *why*.

    ```bash
    git add .
    git commit -m "feat: Add support for YAML OpenAPI specifications"
    ```

6.  **Push to Your Fork**: Push your new branch to your forked repository on GitHub.

    ```bash
    git push origin your-branch-name
    ```

7.  **Submit a Pull Request (PR)**:
    *   Go to the original `RestApiGenerator` repository on GitHub.
    *   You should see a prompt to create a new pull request from your recently pushed branch.
    *   Provide a clear and detailed description of your changes in the pull request. Explain the problem your PR solves, the solution you implemented, and any relevant context.
    *   Reference any related issues (e.g., `Fixes #123`, `Closes #456`).

## Code of Conduct

Please note that this project is released with a Contributor Code of Conduct. By participating in this project, you agree to abide by its terms.

Thank you for contributing to `RestApiGenerator`!
