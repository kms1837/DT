create table skills (
    id int NOT NULL Primary Key AUTOINCREMENT,
    name VARCHAR,
    description VARCHAR,
    before_delay FLOAT,
    after_delay FLOAT,
    range FLOAT,
    power FLOAT,
    cool_time FLOAT,
    effect FLOAT,
    script VARCHAR
);