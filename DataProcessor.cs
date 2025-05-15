public delegate void DataProcessor<T>(ref T data, Actor actor, Entity target) where T : struct;
