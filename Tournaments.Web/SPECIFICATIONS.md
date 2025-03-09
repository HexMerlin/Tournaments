# Tournaments.Web Application Specifications

## Project Boundaries
- This document outlines the specifications for the Tournaments.Web application
- Only the Tournaments.Web project will be modified
- All other projects (Tournaments.Api, Tournaments.Shared) remain unchanged
- The web application must strictly adhere to the existing API endpoints

## Application Purpose
- This is a one-off application for demonstration purposes only
- It is not a product, nor will it be used for actual tournaments
- This implies the following:
  1. Keep things simple and never complicate more than needed for the given purpose
  2. Focus should be on elegance, leanness, and user-friendliness
  3. Not on genericity or support for any future changes or extensions
  4. Neither the backend API code nor the Web App will be changed once completed
  5. No need to worry about long-term maintenance or future technology compliance

## Project References and Dependencies
- The Tournaments.Shared project is referenced by Tournaments.Web
- The web application must fully exploit these shared classes internally
- No replication of model classes is allowed - use the same classes as the API
- This approach ensures consistency between API and client while reducing maintenance overhead
- Using shared models simplifies serialization/deserialization of API responses

## API Integration Approach
- The web application must consume API responses appropriately
- The web application is purpose-built exclusively for this API and domain
- The web application should leverage domain knowledge rather than relying on HATEOAS discovery
- Direct knowledge of API endpoints and structure is expected and encouraged
- During development, the API should be inspected by looking at code and documentation in Tournaments.Api and Tournaments.Shared
- The web application should be optimized for user experience, not for API flexibility

## Data Model Requirements
- Player Model:
  - Gamertag (unique, case-sensitive, primary key)
  - Name (modifiable)
  - Age (integer, 1-200, modifiable)
- Tournament Model:
  - Name (unique identifier, primary key)
  - ParentTournament (optional, modifiable)
  - Support for up to 5 levels of nesting
- Registration Model:
  - Links Tournament and Player
  - Enforces parent tournament registration requirements

## Data Constraints
- Tournament Deletion:
  - Cascades to all sub-tournaments
  - Removes all player registrations in affected tournaments
- Player Deletion:
  - Removes player from all tournament registrations
- Registration Rules:
  - Players must be registered in parent tournament before sub-tournament registration
  - Removing from parent tournament removes from all sub-tournaments

## Communication Protocol
- When encountering unclear or ambiguous requirements, the AI must:
  1. Pause development work
  2. Submit a clear question to the human supervisor
  3. Wait for clarification before resuming work
  4. Document the clarification in this specification document if it results in changes

## Specification Management
- This is a living document that may evolve during development
- The AI must:
  1. Stay updated with any changes made to this document
  2. Adapt development work according to updated specifications
  3. Document any significant changes that affect ongoing work

## Action Reference Protocol
- When performing development work, the AI must:
  1. Reference specific sections of this specification that apply to the current task
  2. Explain how the work aligns with the referenced specifications
  3. This helps maintain mutual understanding between AI and supervisor about:
     - Why certain decisions are made
     - How features are implemented
     - What requirements are being addressed

## Change Request Process
If changes outside Tournaments.Web project are deemed necessary, the following process must be followed:
1. Submit a formal request to the human supervisor that includes:
   - Exact description of what needs to be changed
   - Detailed explanation of why the change is necessary
2. Wait for explicit approval before proceeding with any changes
3. If the request is denied, implement the alternative solution provided by the supervisor
4. Document any approved changes in this specification document

## API Integration
- The web application communicates with Tournaments.Api using HTTP client
- All API responses must be properly handled with appropriate error states
- Data structures and validation rules must match the API specifications
- API endpoints are defined in Tournaments.Api/Tournaments.Api.http

## Core Features

### Player Management
- Create new players
- View player details
- Update player information
- Delete players
- List all players

### Tournament Management
- Create new tournaments
- View tournament details
- Update tournament information
- Delete tournaments
- Handle tournament hierarchy
  - Create sub-tournaments
  - View tournaments with/without sub-tournaments
  - Maintain parent-child relationships

### Registration Management
- Register players in tournaments
- Handle registration constraints
  - Parent tournament requirements
  - Duplicate registration prevention
- Remove registrations
- View registration details

## Development Approach
- The development process will follow a functionality-first approach:
  1. Phase 1: Implement all required functionality with basic, rudimentary UI styling
     - Focus on ensuring all features work correctly
     - Ensure proper API integration
     - Implement all required business logic
     - Basic, functional UI elements only
  2. Phase 2: Enhance UI design and styling after all functionality is complete
     - Implement polished visual design
     - Add animations and transitions
     - Improve overall user experience
     - Refine responsive layouts
- This approach ensures core functionality is prioritized before investing time in visual enhancements

## Initial Example Pages
- The initial state of the Web App contains example pages (counter, weather forecast)
- These example pages:
  - Are not part of the final web application
  - Are initially useful to verify the application is running correctly
  - Serve as reference for Blazor component structure and patterns
  - Should be kept until custom UI components are implemented and working
  - Can be removed once equivalent tournament management functionality is in place
- The removal of example pages should be done incrementally as new features are implemented

## Technical Requirements

### UI/UX
- Implement using Blazor components
- Follow responsive design principles
- Provide clear feedback for user actions
- Implement loading states
- Handle errors gracefully
- Include a textual widget to display server responses with good formatting
  - This will assist during development
  - Enable displaying API reachability information
  - Show database status messages
  - Present raw API responses when needed for debugging

### State Management
- Implement proper state management for application data
- Handle API communication states
- Maintain consistent UI state

### Navigation
- Implement proper routing
- Provide clear navigation between features
- Maintain browser history

### Error Handling
- Display user-friendly error messages
- Handle API errors appropriately
- Provide recovery options when possible

## Additional Implementation Requirements

### Project Structure and Organization
- Use latest established and standardized structure for Blazor WebAssembly apps
- Follow modern .NET and C# conventions
- Organize code according to standard practices for Blazor applications
- Use appropriate separation of concerns (components, services, etc.)

### API URL Configuration
- API base URL should be hardcoded in standardized configuration file(s)
- Follow established practices for configuration in Blazor WebAssembly
- Support different configurations (development, production) as in the API backend

### Development Workflow Process
- The AI should demonstrate changes after every relevant new subtask
- Distinct subtasks should be demonstrated and reviewed separately
- Example: 
  1. Implementing a completely new component (e.g., text widget) is one subtask
  2. Adding several new buttons to an existing page is another subtask
- Similar or related changes (e.g., adding multiple buttons of the same type) can be grouped

### Security Considerations
- No additional security implementations required initially
- Data in the database is considered publicly available
- Read operations do not require authentication
- Simple authentication may be added later for operations that modify the database

### Environment Configuration
- Use standard approach aligned with the rest of the solution
- The web app should align with API's environment detection
- Follow the same pattern as the API which uses IWebHostEnvironment.IsDevelopment()
- Either access this method directly or use the underlying configuration

### Testing Requirements
- Primary testing approach: supervisor-driven manual testing
- The AI should ensure the application can be started and demonstrated to the supervisor
- The AI must verify compilation succeeds with no errors or severe warnings
- The AI may suggest complementary testing strategies as development progresses

### Browser Compatibility
- Primary support: Chrome latest version
- General support for modern browsers only
- Browser-specific issues will be addressed if/when they arise
- Initial focus is on the main browser (Chrome)

### Documentation
- Specific C# coding guidelines including documentation will be provided separately
- Documentation practices will adhere to these guidelines once provided

### Error Handling and Logging
- Errors should be displayed in the text widget specified in UI/UX requirements
- No additional error logging infrastructure required at this stage

### Performance Requirements
- Implement using modern standard practices for the tech stack
- No specific performance benchmarks required
- Standard performance optimization practices are sufficient

## Development Rules
1. No modifications to Tournaments.Api or Tournaments.Shared projects
2. All API calls must use the defined endpoints
3. Follow the existing data models and validation rules
4. Implement proper error handling for all operations
5. Maintain consistent UI/UX patterns throughout the application 