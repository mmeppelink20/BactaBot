CREATE TYPE UsersTableType AS TABLE
(
    user_id BIGINT PRIMARY KEY,
    username NVARCHAR(255) NOT NULL,
    is_bot BIT NOT NULL,
    avatar_url NVARCHAR(1000) NULL
);
GO