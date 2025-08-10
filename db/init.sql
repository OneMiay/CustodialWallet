-- Initial schema for CustodialWallet
CREATE TABLE IF NOT EXISTS public.users (
  id uuid PRIMARY KEY,
  email text NOT NULL UNIQUE,
  balance numeric(38,18) NOT NULL
);


