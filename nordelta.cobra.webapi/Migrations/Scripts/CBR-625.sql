-- Eliminamos account balances con cuits extrajeros actuales
DELETE FROM AccountBalances
WHERE ClientCuit LIKE '5%' AND ClientReference IS NULL
