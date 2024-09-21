## Twitter Clone API

### Overview
Twitter Clone API is a fully functional backend API built using **ASP.NET Core**, designed to replicate key functionalities of Twitter. This API supports user authentication, tweet creation, commenting, liking, and real-time updates using **SignalR**. It is structured using **Clean Architecture** principles, ensuring maintainability, scalability, and separation of concerns.

### Technologies Used:
- **ASP.NET Core Web API**: Core framework for building the API.
- **Entity Framework Core**: ORM for database management and queries.
- **SQL Server**: Relational database used for storing data.
- **JWT & Refresh Token**: For secure user authentication and session management.
- **LINQ**: Used for querying the database in a more readable manner.
- **SignalR**: For real-time updates of likes, comments, notifications, followers, and more.
- **Logging (Serilog)**: For structured logging and tracking system events.
- **ASP.NET Identity**: For managing user roles and authentication.

### Code Architecture:
- **Clean Architecture**: To ensure a well-structured and maintainable codebase by following separation of concerns and dependency inversion principles.
- **N-Tier Architecture**: Implementing separate layers for data access, business logic, and presentation.
- **Onion Architecture**: Managing dependency inversions, ensuring a loosely coupled system.
- **Unit of Work**: To manage transactions and ensure data consistency across operations.
- **Generic Repository**: Provides a reusable way to perform CRUD operations.

### Features:
- **User Authentication**: Registration, login, role management, and JWT-based authentication.
- **Tweets**: Users can create tweets, retweets, and manage them.
- **Comments**: Users can comment on tweets and reply to comments.
- **Likes**: Like or unlike tweets and comments.
- **Genres**: Tweets can be categorized into specific genres.
- **Profiles**: Users can manage their profiles with profile pictures and personal details.
- **Notifications**: Real-time notification system for likes, comments, and followers.
- **User Connections**: Follow and unfollow users with real-time follower updates.

### Real-time Features:
Using **SignalR**, real-time updates are provided for:
- Total comments on tweets.
- Total likes on tweets and comments.
- Total unread notifications.
- Follower and following updates.

### API Endpoints:

#### Account:
- `POST /api/Account/register`: Register a new user.
- `POST /api/Account/login`: User login.
- `POST /api/Account/addrole`: Add a new role.
- `POST /api/Account/refreshtoken`: Refresh JWT token.
- `POST /api/Account/revoketoken`: Revoke an active token.
- `POST /api/Account/forgotpassword`: Request password reset.
- `POST /api/Account/resetpassword`: Reset user password.

#### Comment:
- `POST /api/Comment/CreateComment`: Create or reply to a comment.
- `GET /api/Comment/getComments`: Retrieve all comments.
- `GET /api/Comment/getCommentsForTweet/{tweetID}`: Get comments for a specific tweet.
- `GET /api/Comment/getRepliesForComment/{commentId}`: Get replies for a specific comment.
- `GET /api/Comment/getComment/{commentId}`: Retrieve a specific comment.
- `PUT /api/Comment/UpdateComment`: Update an existing comment.
- `DELETE /api/Comment/DeleteComment/{commentID}`: Delete a comment.

#### Genre:
- `GET /api/Genre/getGenres`: Retrieve all genres.
- `GET /api/Genre/getGenre/{genreId}`: Retrieve a specific genre.
- `POST /api/Genre/createGenre`: Create a new genre.
- `PUT /api/Genre/updateGenre`: Update an existing genre.
- `DELETE /api/Genre/deleteGenre/{genreId}`: Delete a genre.

#### Like:
- `POST /api/Like/likeTweetOrComment/{id}`: Like a tweet or comment.
- `POST /api/Like/UnlikeTweetOrComment/{id}`: Unlike a tweet or comment.
- `GET /api/Like/getLikesFortweetOrComment/{id}`: Get all likes for a tweet or comment.
- `GET /api/Like/userIsLiked/{id}`: Check if the user has liked the tweet or comment.

#### Notification:
- `GET /api/Notification/getNotificationsForProfile/{profileID}`: Retrieve notifications for a user profile.
- `GET /api/Notification/getNotification/{notificationID}`: Retrieve a specific notification and mark it as read.
- `DELETE /api/Notification/deleteNotification/{notificationID}`: Delete a specific notification.
- `PUT /api/Notification/markAllAsRead/{profileID}`: Mark all notifications as read for a user.

#### Profile:
- `POST /api/Profile/createProfile`: Create a user profile.
- `PUT /api/Profile/updateProfile`: Update a user profile.
- `DELETE /api/Profile/deleteProfile/{id}`: Delete a profile.
- `GET /api/Profile/getProfile/{id}`: Retrieve a specific profile.
- `GET /api/Profile/getProfileByUserId/{userId}`: Get a profile by user ID.
- `GET /api/Profile/getProfiles`: Retrieve all profiles.

#### Tweet:
- `POST /api/Tweet/createTweet`: Create a tweet or retweet.
- `PUT /api/Tweet/updateTweet`: Update an existing tweet.
- `DELETE /api/Tweet/deleteTweet/{tweetID}`: Delete a tweet.
- `GET /api/Tweet/getTweets`: Get all tweets.
- `GET /api/Tweet/getTweet/{tweetID}`: Get a specific tweet.
- `GET /api/Tweet/getTweetsForSpecificProfile/{profileID}`: Get all tweets for a specific profile.
- `GET /api/Tweet/getTweetsForSpecificGenre/{genreID}`: Get all tweets for a specific genre.

#### User Connections:
- `POST /api/UserConnections/Follow/{followedId}`: Follow a user.
- `POST /api/UserConnections/Unfollow/{UnfollowedId}`: Unfollow a user.
- `GET /api/UserConnections/Following/{profileId}`: Get all users that the profile is following.
- `GET /api/UserConnections/Followers/{profileId}`: Get all followers of a profile.

### Logging:
The application uses **Serilog** for structured logging. Logs capture errors, exceptions, and important events throughout the API.

## Diagram:
![image](https://github.com/user-attachments/assets/af1c2942-d321-478c-9fe8-798a42c34599)

