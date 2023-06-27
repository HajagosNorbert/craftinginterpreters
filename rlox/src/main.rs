use std::fmt;
use std::vec;

mod Lox {
    #[derive(Debug)]
    pub enum OpCode {
        OpReturn,
    }

    // impl fmt::Debug for Vec<OpCode> {
    //     fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
    //         f.debug_struct("Vec").field("buf", &self.buf).field("len", &self.len).finish()
    //     }
    // }
}


fn main() {
    let mut chunk: Vec<Lox::OpCode> = Vec::new();
    // chunk.push(OpCode::OpReturn);
    // chunk.push(OpCode::OpReturn);
    println!("{:?}", chunk);
}
