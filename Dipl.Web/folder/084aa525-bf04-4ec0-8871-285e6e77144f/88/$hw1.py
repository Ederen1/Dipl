from damgard_jurik import keygen, PublicKey, EncryptedNumber
from random import randint

public_key, private_key_ring = keygen(
    n_bits=64,
    s=1,
    threshold=3,
    n_shares=3
)

# Debug board with unencrypted keys
debug = []

board = []

def post_vote():
    # Randomly choose vote
    mi = randint(0, 1)
    debug.append(mi)

    ci = public_key.encrypt(mi)
    board.append(ci)

def count_votes():
    c = board[0]
    for vote in board[1:]:
        c += vote
    
    return c

for voter in range(0, 50):
    post_vote()

c = count_votes()

m = private_key_ring.decrypt(c)

print(f"m: {m}")
print(f"debug {sum(debug)}")
