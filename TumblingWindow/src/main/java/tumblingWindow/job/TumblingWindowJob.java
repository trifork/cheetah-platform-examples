package tumblingWindow.job;

import com.trifork.cheetah.processing.util.WatermarkStrategyBuilder;
import org.apache.flink.api.common.eventtime.WatermarkStrategy;
import org.apache.flink.streaming.api.windowing.assigners.TumblingEventTimeWindows;
import org.apache.flink.streaming.api.windowing.time.Time;
import tumblingWindow.model.TumblingWindowInputEvent;
import tumblingWindow.model.TumblingWindowOutputEvent;
import com.trifork.cheetah.processing.connector.kafka.KafkaDataStreamBuilder;
import com.trifork.cheetah.processing.connector.kafka.KafkaSinkBuilder;
import com.trifork.cheetah.processing.connector.serialization.SimpleKeySerializationSchema;
import com.trifork.cheetah.processing.job.Job;
import org.apache.flink.connector.kafka.sink.KafkaSink;
import org.apache.flink.connector.kafka.source.enumerator.initializer.OffsetsInitializer;
import org.apache.flink.streaming.api.datastream.DataStream;
import org.apache.flink.streaming.api.datastream.SingleOutputStreamOperator;

import java.io.Serializable;
import java.time.Instant;

/** TumblingWindowJob sets up the data processing job. */
public class TumblingWindowJob extends Job implements Serializable {

    @SuppressWarnings("PMD.SignatureDeclareThrowsException") // Fix once lib-processing is fixed
    public static void main(final String[] args) throws Exception {
        new TumblingWindowJob().start(args);
    }

    @Override
    protected void setup() {
        // Input source
        final DataStream<TumblingWindowInputEvent> inputStream =
                KafkaDataStreamBuilder.forSource(this, TumblingWindowInputEvent.class)
                        .offsetsInitializer(OffsetsInitializer.earliest())
                        .build();

        // Transform stream
        final WatermarkStrategy<TumblingWindowInputEvent> watermarkStrategy = WatermarkStrategyBuilder
                .builder(TumblingWindowInputEvent.class)
                .eventTimestampSupplier(input -> Instant.ofEpochMilli(input.getTimestamp()))
                .build();

        final SingleOutputStreamOperator<TumblingWindowOutputEvent> outputStream = inputStream
                .assignTimestampsAndWatermarks(watermarkStrategy)
                .keyBy(TumblingWindowInputEvent::getDeviceId)
                .window(TumblingEventTimeWindows.of(Time.minutes(5)))
                .process(new TumblingWindowMapper());

        // Output sink
        final KafkaSink<TumblingWindowOutputEvent> kafkaSink =
                KafkaSinkBuilder.defaultKafkaConfig(this, TumblingWindowOutputEvent.class)
                        .keySerializationSchema(
                                new SimpleKeySerializationSchema<>() {

                                    @Override
                                    public Object getKey(final TumblingWindowOutputEvent outputEvent) {
                                        return outputEvent.getDeviceId();
                                    }
                                })
                        .build();

        // Connect transformed stream to sink
        outputStream.sinkTo(kafkaSink).name(TumblingWindowJob.class.getSimpleName());
    }
}
