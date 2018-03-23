module utils

let merge (f : 'a -> 'b * 'b -> 'b) (a : Map<'a, 'b>) (b : Map<'a, 'b>) =
    Map.fold (fun s k v ->
        match Map.tryFind k s with
        | Some v' -> Map.add k (f k (v, v')) s
        | None -> Map.add k v s) a b
