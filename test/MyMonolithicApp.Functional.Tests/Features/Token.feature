Feature: Authenticate users

  Scenario: User is authenticated successfully
	Given User exists in database
	When I submit the user's credentials
	Then I receive the token response

  Scenario: User authentication fails
	Given User exists in database
	When I submit wrong user's credentials
	Then I receive an authentication failed error