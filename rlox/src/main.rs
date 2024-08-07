use std::fmt;
use std::vec;

#[derive(Debug)]
enum OpCode {
    OpReturn,
}

struct Chunk {
    codes: Vec<OpCode>
}

// impl fmt::Debug for Vec<OpCode> {
//     fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
//         f.debug_struct("Vec").field("buf", &self.buf).field("len", &self.len).finish()
//     }
// }

fn main() {
    let mut chunk: Vec<OpCode> = Vec::new();
    // chunk.push(OpCode::OpReturn);
    // chunk.push(OpCode::OpReturn);
    println!("{:?}", chunk);
}
