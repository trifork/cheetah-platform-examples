package cheetah.example.job;

import cheetah.example.model.json.InputEvent;
import org.junit.jupiter.api.Test;

import java.util.UUID;

import static org.junit.jupiter.api.Assertions.assertEquals;

class JsonToAvroMapperTest {
    private final JsonToAvroMapper mapper = new JsonToAvroMapper("extraFieldValue");

    @Test
    public void testEnrichment() {
        final String deviceId = UUID.randomUUID().toString();
        final double value = 1.0;
        final long timestamp = 0;
        final var input = new InputEvent(deviceId, value, timestamp);

        final var actual = mapper.map(input);

        assertEquals(deviceId, actual.getDeviceId());
        assertEquals(value, actual.getValue());
        assertEquals(timestamp, actual.getTimestamp());
        assertEquals("extraFieldValue", actual.getExtraField());
    }
}
