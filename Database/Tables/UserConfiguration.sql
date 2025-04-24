CREATE TABLE UserConfiguration (
    user_id BIGINT NOT NULL,
    config_key NVARCHAR(50) NOT NULL,
    config_value NVARCHAR(MAX) NOT NULL,
    CONSTRAINT PK__UserConfig_UserConfig PRIMARY KEY (user_id, config_key),
    CONSTRAINT FK_UserConfig_user_config FOREIGN KEY (user_id) REFERENCES Users(user_id)
);