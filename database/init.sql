CREATE TABLE IF NOT EXISTS account(
    id serial primary key,
    uid uuid default gen_random_uuid(),
    fullname text not null,
    email text not null
);

CREATE TABLE IF NOT EXISTS expenses(
    id serial primary key,
    uid uuid default gen_random_uuid(),
    user_id int4 not null constraint user_expense_fk references account, 
    name text not null,
    type text not null,
    description text,
    expense_date date,
    creation_date timestamp default now()::timestamp
);